/// <binding BeforeBuild='bower:install, less' />
module.exports = function (grunt) {
    'use strict';

    grunt.loadNpmTasks('grunt-bower-task');
    //grunt.loadNpmTasks('grunt-contrib-concat');
    grunt.loadNpmTasks('grunt-contrib-clean');
    grunt.loadNpmTasks('grunt-contrib-less');
    grunt.loadNpmTasks('grunt-contrib-watch');

    grunt.loadNpmTasks('grunt-contrib-connect');
    grunt.loadNpmTasks('grunt-contrib-uglify');

    grunt.registerTask('server', ['connect:toprak', 'watch:toprak']);

    grunt.option("force", true);

    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),
        bower: {
            install: {
                options: {
                    targetDir: './wwwroot/lib',
                    layout: 'byComponent',
                    cleanTargetDir: true,
                    install: true,
                    verbose: true
                }
            }
        },
        connect: {
            toprak: {
                options: {
                    port: 9000,
                    base: 'wwwroot',
                    keepalive: true
                }
            }
        },
        uglify: {},
        watch: {
            toprak: {
                // '**' is used to include all subdirectories
                // and subdirectories of subdirectories, and so on, recursively.
                files: ['wwwroot/**/*'],
                tasks: []
            }
        }
    });
};