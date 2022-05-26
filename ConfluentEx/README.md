This is working example for consumer/producer that works both for Azure Event Hub and Confluent Cloud Cluster (code is taken from official Confluent https://developer.confluent.io/get-started/dotnet/#kafka-setup).  

The only thing needs to change is properties files. It was tested for multiple instances of consumers with same groupid as well as with different groupid, in parallel. 
