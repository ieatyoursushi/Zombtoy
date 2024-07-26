const express = require("express");
const fs = require("fs");
const server = express();
const port: number = 3000;
//let thisnum: number = 30;
//npx tsc example.ts -> converts to js file

server.listen(port, (error) => {
    if(!error) {
        console.log(`server running on ${port}`)
    } else {
        console.log("Server Error");
    }
});

server.get('/', (req, res)=>{
    res.send("Welcome to root URL of Server");
});

server.use(express.text());
server.post('/addScore', (req, res)=> {
    //makes sure data can smoothly be written to the csv file database.
    const score: string = req.body;
    let scoreJson: ScoreJson = new ScoreJson(score);
    console.log("score received: " + score);
    console.log(JSON.stringify(scoreJson));
    fs.appendFile('scores.csv', JSON.stringify(scoreJson) + ",", (err) => {
        if (err) {
            console.error('Error writing to file');
            res.status(422).send("partial failure: failed to store score");
        } else {
            console.log("written to file successfully");
            res.status(200).send("score received and stored: " + score);
        }
    })
    
})
server.get('/getAllScores', (req, res) => {
    fs.readFile('scores.csv', 'utf8', (err, data) => {
        if (err) {
          console.error(err);
          return;
        }
        const scoreArray: string = `[${data.slice(0, -1)}]`;
        const parsedScoreArray: any = JSON.parse(scoreArray);
        console.log(parsedScoreArray);
        let scoresArray: string[] = [];
        let scores: string= "";
        for(let i = 0; i < parsedScoreArray.length; i++) {
            scores += (parsedScoreArray[i].score + ",")
        }
        res.send(scores.slice(0, -1));

      });
});
 //killall -9 node
 class ScoreJson {
    private score: string;
    constructor(score: string) {
        this.score = score;
    }
 }