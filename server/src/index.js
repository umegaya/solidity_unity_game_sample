var express = require ('express');
var body_parser = require('body-parser');
var functions = require('./functions');

var app = express();
app.use(body_parser.json());

var router = express.Router();
for (var k in functions) {
	console.log("expose function:" + k);
	router.post('/' + k, functions[k]);
}
app.use('/functions', router);

app.listen(5000);
