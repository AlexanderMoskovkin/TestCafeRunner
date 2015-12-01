//Parse arguments
var testCafeDir = process.argv[2];
var testsDir = process.argv[3];
var browsers = JSON.parse('"' + process.argv[4] + '"');


var path = require('path');
var glob = require('glob');
var createTestCafe = require(path.join(testCafeDir, 'lib'));


browsers = browsers.map(function (browser) {
    return browser.alias || browser;
});

glob(path.join(testsDir, '**', '*.test.js'), function (err, testFiles) {
    createTestCafe('127.0.0.1', 1337, 1338)
        .then(function (testCafe) {
            var runner = testCafe.createRunner();

            runner
                .browsers(browsers)
                .reporter('json', process.stdout)
                .src(testFiles)
                .run()
                .then(function (failed) {
                    process.exit(failed ? -1 : 0);
                })
                .catch(function () {
                    process.exit(-1);
                });
        });
});