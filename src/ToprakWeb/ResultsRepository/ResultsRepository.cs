namespace ToprakWeb.ResultsRepository
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using Dapper;

    using ImageManager.Model;

    using Microsoft.Framework.Logging;
    using Microsoft.SqlServer.Server;
    using ToprakWeb.ImageManager;

    public class ResultsRepository : BaseDatabaseRepository, IResultsRepository
    {
        private readonly ILogger logger;

        public ResultsRepository(string connectionString, ILogger logger) : base(connectionString)
        {
            this.logger = logger;
        }

        public async Task<IEnumerable<GorulmusTutanakMesaji>> GetTutanakResultsAsync(string imageId)
        {
            return await this.WithConnection(
                async c =>
                    {
                        try
                        {
                            var parameters = new DynamicParameters();
                            parameters.Add("@ImageId", imageId, DbType.AnsiString);
                            var results =
                                await
                                    c.QueryAsync<GorulmusTutanakMesaji>(
                                        sql: "select * from GORULMUSTUTANAK where IMAGE = @ImageId",
                                        param: parameters,
                                        commandType: CommandType.Text);
                            return results;
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e.ToString());
                            return default(IEnumerable<GorulmusTutanakMesaji>);
                        }
                    });
        }

        public async Task<UserStat> GetUserStats(string email)
        {
            var result = await this.WithConnection(
            async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Email", email, DbType.AnsiString);
                var results =
                                await
                                c.QueryAsync<UserStat>(
                                    sql: "spUserStats",
                                    param: parameters,
                                    commandType: CommandType.StoredProcedure);
                return results;
            });
            return result.FirstOrDefault();
        }

        public async Task RecordResult(TutanakDataEnvelope envelope)
        {
            UserLocation location = null;

            try
            {
                
                var reqeust = WebRequest.Create($"http://freegeoip.net/json/{envelope.UserRecord.Ip}");
                var response = await reqeust.GetResponseAsync();
                var stream = response.GetResponseStream();

                if (stream != null)
                {
                    using (var sr = new StreamReader(stream))
                    {
                        var result = sr.ReadToEnd();
                        location = UserLocation.Build(result);
                    }
                }
            }
            catch (Exception exception)
            {

                this.logger.LogError($"Error getting the location {exception.ToString()}");
            }

            await this.WithConnection(
                async c =>
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(envelope.UserRecord.Email))
                            {
                                envelope.UserRecord.Email = "noemail";
                            }
                            if (string.IsNullOrEmpty(envelope.TutanakData.SeenBy))
                            {
                                envelope.TutanakData.SeenBy = "noemail";
                            }
                            var parameters = this.buildParameters(envelope);
                            if (location != null)
                            {
                                parameters.AddLocation(location);
                            }

                            await c.ExecuteAsync(@"spRecordResult", parameters, commandType: CommandType.StoredProcedure);
                        }
                        catch (Exception e)
                        {
                            this.logger.LogError(e.ToString());
                        }

                    });
        }

        public async Task<IEnumerable<TemsilcilikSpec>> GetTemsilcilikler(CancellationToken cancellationToken)
        {
            return
                await
                this.RunSelectStatementWithNoParams<TemsilcilikSpec>("SELECT * FROM TEMSILCILIKLER", cancellationToken);
        }

        public async Task RecordReadSuccessResult(TutanakDataEnvelope envelope)
        {
            await this.WithConnection(
                async c =>
                    {
                        try
                        {
                            var parameters = (new DynamicParameters()).AddTutanakInfo(envelope.TutanakData);

                            await
                                c.ExecuteAsync(@"spRecordSuccess", parameters, commandType: CommandType.StoredProcedure);
                        }
                        catch (Exception e)
                        {
                            this.logger.LogError(e.ToString());
                        }

                    });
        }

        public async Task<IEnumerable<ImageReadCount>> GetTopSeenImages(CancellationToken cancellationToken)
        {
            return
                await
                this.RunSelectStatementWithNoParams<ImageReadCount>(
                    "SELECT * FROM MORETHAN4VIEWS ORDER BY [COUNT] DESC, [IMAGE] ASC",
                    cancellationToken);
        }

        public async Task<IEnumerable<ImageReadCount>> GetUnreadableImages(CancellationToken cancellationToken)
        {
            return
                await
                this.RunSelectStatementWithNoParams<ImageReadCount>(
                    "SELECT * FROM NOTREADABLE ORDER BY [COUNT] DESC, [IMAGE] ASC",
                    cancellationToken);
        }

        public async Task<IEnumerable<LocationStats>> GetUserLocations(CancellationToken cancellationToken)
        {
            return
                await
                this.RunSelectStatementWithNoParams<LocationStats>(
                    "SELECT * FROM AnalizLocation",
                    cancellationToken);
        }

        public async Task<IEnumerable<User>> GetTopGonullu(CancellationToken cancellationToken)
        {
            return
                await
                this.RunSelectStatementWithNoParams<User>(
                    "SELECT * FROM TOPNUSERS",
                    cancellationToken);
        }

        private async Task<IEnumerable<T>>  RunSelectStatementWithNoParams<T>(string selectText,CancellationToken cancellationToken) where T: IDbResult
        {
            try
            {
                var result = await this.WithConnection(
                    async c =>
                        {
                            var results = await c.QueryAsync<T>(sql: selectText, commandType: CommandType.Text);
                            return results;
                        });
                return result;
            }
            catch (Exception e)
            {
                this.logger.LogCritical(e.ToString());
            }
            return default(IEnumerable<T>);
        }

        private DynamicParameters buildParameters(TutanakDataEnvelope envelope)
        {
            if (string.IsNullOrEmpty(envelope.UserRecord.Email))
            {
                envelope.UserRecord.Email = string.IsNullOrEmpty(envelope.TutanakData.SeenBy) ? "noemail" : envelope.TutanakData.SeenBy;
            }

            var parameters = new DynamicParameters();

            parameters = parameters.AddUserInfo(envelope.UserRecord).AddTutanakInfo(envelope.TutanakData);
            return parameters;
        }
    }
}
