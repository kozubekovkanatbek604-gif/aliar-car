#!/bin/sh
set -e

mkdir -p /data/uploads/cars /data/uploads/managers

if [ -d /app/wwwroot/uploads ] && [ ! -L /app/wwwroot/uploads ]; then
  rm -rf /app/wwwroot/uploads
fi

if [ ! -e /app/wwwroot/uploads ]; then
  ln -sfn /data/uploads /app/wwwroot/uploads
fi

exec dotnet /app/Aliyar.Web.dll
