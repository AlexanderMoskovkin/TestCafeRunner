var fs = require('fs');
var Mustache = require('mustache');

function successToResult(success) {
    return success ? 'Success' : 'Failure';
}

function successToString(success) {
    return success ? 'True' : 'False';
}

function numberToString(number) {
    return number < 10 ? '0' + number : number.toString();
}

function parseTime(date) {
    return [
        numberToString(date.getHours()),
        numberToString(date.getMinutes()),
        numberToString(date.getSeconds())
    ].join(':');
}

function convert(jsonReport) {
    var results = JSON.parse(jsonReport);

    var startDate = new Date(results.startTime);
    var endDate = new Date(results.endTime);
    var success = (results.total - results.passed) === 0;

    var resForNUnit = {
        total: results.total,
        failures: results.total - results.passed,
        date: endDate.getFullYear() + '-' + (endDate.getMonth() + 1) + '-' + endDate.getDate(),
        time: parseTime(endDate),
        runTime: (endDate.getTime() - startDate.getTime()) / 1000,
        result: successToResult(success),
        success: successToString(success),
        fixtures: []
    };

    resForNUnit.fixtures = results.fixtures.map(function (fixture) {
        var fixtureSuccess = true;

        var tests = fixture.tests.map(function (test) {
            var testSuccess = !test.errs.length;

            if (!testSuccess)
                fixtureSuccess = false;

            return {
                name: test.name,
                time: test.durationMs / 1000,
                result: successToResult(testSuccess),
                success: successToString(testSuccess),
                errs: test.errs
            };
        });

        var fixtureForNUnit = {
            name: fixture.path,
            time: 0,
            result: successToResult(fixtureSuccess),
            success: successToString(fixtureSuccess),
            tests: tests
        };

        fixtureForNUnit.tests.forEach(function (test) {
            fixtureForNUnit.time += test.time;
        });

        return fixtureForNUnit;
    });

    return Mustache.render(fs.readFileSync('./js/json-to-nunit.mustache', 'utf-8'), resForNUnit);
}

process.stdin.setEncoding('utf8');

process.stdin.on('data', function (jsonReport) {
    //NOTE: remove BOM
    jsonReport = jsonReport.replace(/^\uFEFF/, '');

    var nunitReport = convert(jsonReport);

    process.stdout.write(nunitReport);
    process.exit(0);
});

