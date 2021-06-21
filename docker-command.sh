docker build --pull -t covidapi .

docker rm covid_app -f

docker container run -v /home/csv:/app/csv --publish 5000:80 --publish 5001:443 --detach --restart unless-stopped --name covid_app covidapi

