import json
import pymongo
import urllib.request
import pandas as pd
from pathlib import Path
from flask import Flask
from threading import Thread

configurationJson = open('configuration.json').read()
configuration = json.loads(configurationJson)
serviceStatus = 'idle'

def performInit():
	if not Path('data.csv').is_file():
		urllib.request.urlretrieve(configuration['CsvLocation'], 'data.csv')
	dataframe = pd.read_csv('data.csv')
	mongo = pymongo.MongoClient(configuration['MongoDbConnectionString'])
	mongo.drop_database(configuration['DbName'])
	db = mongo[configuration['DbName']]
	col = db[configuration['InitCol']]
	toInsert = []
	for index, row in dataframe.iterrows():
		toInsert.append({'text': row[0], 'item': row[1]})
	col.insert_many(toInsert)
	global serviceStatus
	serviceStatus = 'idle'

app = Flask(__name__)

@app.route("/status")
def status():
	global serviceStatus
	return serviceStatus

@app.route("/doinitdb")
def doinitdb():
	global serviceStatus
	if serviceStatus == 'idle':
		serviceStatus = 'working'
		thread = Thread(target = performInit)
		thread.start()
	return 'ok'

if __name__ == "__main__":
	app.run(host='0.0.0.0', port=6000)
