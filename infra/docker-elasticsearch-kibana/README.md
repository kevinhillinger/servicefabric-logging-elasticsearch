# Deployment of Kibana+Elasticsearch Containers with Docker Compose


<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fkevinhillinger%2Fservicefabric-logging-elasticsearch%2Fmaster%2Finfra%2Fdocker-elasticsearch-kibana%2Fazuredeploy.json" target="_blank">
	<img src="http://azuredeploy.net/deploybutton.png"/>
</a>
<a href="http://armviz.io/#/?load=https%3A%2F%2Fraw.githubusercontent.com%2Fkevinhillinger%2Fservicefabric-logging-elasticsearch%2Fmaster%2Finfra%2Fdocker-elasticsearch-kibana%2Fazuredeploy.json" target="_blank">
    <img src="http://armviz.io/visualizebutton.png"/>
</a>

This template allows you to deploy an Ubuntu Server 15.04 VM with Docker (using the [Docker Extension][ext])
and starts a Kibana container listening on port 5601 which uses Elasticsearch database running
in a separate but linked Docker container, which are created using [Docker Compose][compose]
capabilities of the [Azure Docker Extension][ext].


[ext]: https://github.com/Azure/azure-docker-extension
[compose]: https://docs.docker.com/compose
