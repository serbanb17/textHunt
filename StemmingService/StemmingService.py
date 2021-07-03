import sys
from nltk.stem import PorterStemmer
from flask import Flask

porter = PorterStemmer()
app = Flask(__name__)

@app.route("/<word>")
def hello(word):
    return porter.stem(word)

if __name__ == "__main__":
    app.run(host='0.0.0.0', port=sys.argv[1])
