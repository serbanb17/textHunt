FROM python:3.5.4
COPY . .
RUN pip install pandas==0.25.3 pymongo==3.11.0 flask==1.1.2
ENTRYPOINT python ./DbInitService.py
