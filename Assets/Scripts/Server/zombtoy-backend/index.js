const express = require("express");
const fs = require("fs");
const server = express();
const port = 3000;
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
server.post('/addScore', (req, res)=> {
 
})
server.get('/getAllScores', (req, res) => {
    fs.readFile('scores.csv', 'utf8', (err, data) => {
        if (err) {
          console.error(err);
          return;
        }
        const scoreArray = `[${data.slice(0, -1)}]`;
        const parsedScoreArray = JSON.parse(scoreArray);
        console.log(parsedScoreArray);
        let scoresArray = [];
        for(let i = 0; i < parsedScoreArray.length; i++) {
            scoresArray[i] = parsedScoreArray[i].score;
        }
        res.send(scoresArray);

      });
});
 //killall -9 node