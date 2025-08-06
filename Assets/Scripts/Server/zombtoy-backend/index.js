var express = require("express");
var fs = require("fs");
var server = express();
var port = 3000;
//let thisnum: number = 30;
//npx tsc example.ts -> converts to js file
server.listen(port, function (error) {
    if (!error) {
        console.log("server running on ".concat(port));
    }
    else {
        console.log("Server Error");
    }
});
server.get('/', function (req, res) {
    res.send("Welcome to root URL of Server");
});
server.use(express.text());
server.post('/addScore', function (req, res) {
    //makes sure data can smoothly be written to the csv file database.
    var score = req.body;
    var scoreJson = new ScoreJson(score);
    console.log("score received: " + score);
    console.log(JSON.stringify(scoreJson));
    fs.appendFile('scores.csv', JSON.stringify(scoreJson) + ",", function (err) {
        if (err) {
            console.error('Error writing to file');
            res.status(422).send("partial failure: failed to store score");
        }
        else {
            console.log("written to file successfully");
            res.status(200).send("score received and stored: " + score);
        }
    });
});
server.get('/getAllScores', function (req, res) {
    fs.readFile('scores.csv', 'utf8', function (err, data) {
        if (err) {
            console.error(err);
            return;
        }
        var scoreArray = "[".concat(data.slice(0, -1), "]");
        var parsedScoreArray = JSON.parse(scoreArray);
        console.log(parsedScoreArray);
        var scoresArray = [];
        var scores = "";
        for (var i = 0; i < parsedScoreArray.length; i++) {
            scores += (parsedScoreArray[i].score + ",");
        }
        res.send(scores.slice(0, -1));
    });
});
//killall -9 node
var ScoreJson = /** @class */ (function () {
    function ScoreJson(score) {
        this.score = score;
    }
    return ScoreJson;
}());
