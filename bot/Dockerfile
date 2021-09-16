##See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
#
#FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
#WORKDIR /app
#EXPOSE 80
#EXPOSE 443
#
#FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
#WORKDIR /src
#COPY ["QQBot.Web/QQBot.Web.csproj", "QQBot.Web/"]
#COPY ["QQBot.Entities/QQBot.Entities.csproj", "QQBot.Entities/"]
#COPY ["QQBot.Job/QQBot.Job.csproj", "QQBot.Job/"]
#COPY ["QQBot.DB/QQBot.DB.csproj", "QQBot.DB/"]
#COPY ["QQBot.Utils/QQBot.Utils.csproj", "QQBot.Utils/"]
#RUN dotnet restore "QQBot.Web/QQBot.Web.csproj"
#COPY . .
#WORKDIR "/src/QQBot.Web"
#RUN dotnet build "QQBot.Web.csproj" -c Release -o /app/build
#
#FROM build AS publish
#RUN dotnet publish "QQBot.Web.csproj" -c Release -o /app/publish
#
#FROM base AS final
#RUN mv /etc/apt/sources.list /etc/apt/sources.list.bak && \
    #echo "deb http://mirrors.163.com/debian/ buster main non-free contrib" >/etc/apt/sources.list && \
    #echo "deb-src http://mirrors.163.com/debian/ buster main non-free contrib" >>/etc/apt/sources.list && \
    #echo "deb http://mirrors.163.com/debian-security buster/updates main" >>/etc/apt/sources.list && \
    #echo "deb-src http://mirrors.163.com/debian-security buster/updates main" >>/etc/apt/sources.list && \
    #echo "deb http://mirrors.163.com/debian/ buster-updates main non-free contrib" >>/etc/apt/sources.list && \
    #echo "deb-src http://mirrors.163.com/debian/ buster-updates main non-free contrib" >>/etc/apt/sources.list && \
    #echo "deb http://mirrors.163.com/debian/ buster-backports main non-free contrib" >>/etc/apt/sources.list && \
    #echo "deb-src http://mirrors.163.com/debian/ buster-backports main non-free contrib" >>/etc/apt/sources.list
	#
#RUN apt-get update && apt-get install -y nodejs 
#RUN apt-get install -y npm
#
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENV ASPNETCORE_URLS http://*:80
#ENTRYPOINT ["dotnet", "QQBot.Web.dll"]




FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 5000
# 一般情况下必须开放
EXPOSE 80
# 如果使用https，记得打开443端口，但是一般不用
#EXPOSE 443
FROM base AS final
RUN mv /etc/apt/sources.list /etc/apt/sources.list.bak && \
    echo "deb http://mirrors.163.com/debian/ buster main non-free contrib" >/etc/apt/sources.list && \
    echo "deb-src http://mirrors.163.com/debian/ buster main non-free contrib" >>/etc/apt/sources.list && \
    echo "deb http://mirrors.163.com/debian-security buster/updates main" >>/etc/apt/sources.list && \
    echo "deb-src http://mirrors.163.com/debian-security buster/updates main" >>/etc/apt/sources.list && \
    echo "deb http://mirrors.163.com/debian/ buster-updates main non-free contrib" >>/etc/apt/sources.list && \
    echo "deb-src http://mirrors.163.com/debian/ buster-updates main non-free contrib" >>/etc/apt/sources.list && \
    echo "deb http://mirrors.163.com/debian/ buster-backports main non-free contrib" >>/etc/apt/sources.list && \
    echo "deb-src http://mirrors.163.com/debian/ buster-backports main non-free contrib" >>/etc/apt/sources.list
	
RUN apt-get update && apt-get install -y nodejs 
RUN apt-get install -y npm
 
COPY publish/ /app

ENV ASPNETCORE_URLS http://*:80
ENTRYPOINT ["dotnet", "QQBot.Web.dll"]