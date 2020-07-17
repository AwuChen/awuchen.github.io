var express = require('express');
var bodyParser = require('body-parser')

var app = express();

// create application/json parser
var jsonParser = bodyParser.json()

var posX = 0
var posY = 0
var posZ = 0

app.post('/setPos', jsonParser, function (req, res) {
   console.log(req.body)
   console.log(req.body.x + " " + req.body.y + " " + req.body.z)
   posX = req.body.x
   posY = req.body.y
   posZ = req.body.z

   res.end("Ok")

   console.log("setPos called")
})

app.get('/getPos', function (req, res) {
	response = {
		x: posX,
		y: posY,
		z: posZ
	}
   res.end(JSON.stringify(response))
   console.log("getPos called")
})

var server = app.listen(8080, function () {
   var host = server.address().address
   var port = server.address().port
   
   console.log("Example app listening at http://%s:%s", host, port)
})