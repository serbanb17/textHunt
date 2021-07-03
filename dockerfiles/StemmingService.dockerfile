FROM python:3.5.4
ENV appPort=5000
COPY . .
RUN pip install nltk==3.5 flask==1.1.2
ENTRYPOINT python ./StemmingService.py $appPort
