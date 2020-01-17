FROM openjdk:8-jre

# Can not use jre-alpine because of SIGSEGV exception

RUN mkdir /var/dynamodb_local
WORKDIR /var/dynamodb_local

RUN curl -fsSL https://s3-ap-northeast-1.amazonaws.com/dynamodb-local-tokyo/dynamodb_local_latest.tar.gz -o dynamodb.tar.gz \
  && tar xzf dynamodb.tar.gz \
  && rm dynamodb.tar.gz

COPY entrypoint.sh /bin/
RUN chmod 755 /bin/entrypoint.sh

ENTRYPOINT ["/bin/entrypoint.sh"]

EXPOSE 8000
