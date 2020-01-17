#!/bin/sh

set -- java "$@"
set -- "$@" -Djava.library.path=/var/dynamodb_local/DynamoDBLocal_lib
set -- "$@" -jar /var/dynamodb_local/DynamoDBLocal.jar
set -- "$@" -sharedDb
set -- "$@" -dbPath /data
set -- "$@" -port 8000
exec "$@"
