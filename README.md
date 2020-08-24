#AES67 Discovery Server

> The administration of the IdentityServer4 and Asp.Net Core Identity



[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-brightgreen.svg)](COPYING)
[![Build Status](https://dev.azure.com/skoruba/IdentityServer4.Admin/_apis/build/status/IdentityServer4.Admin-CI?branchName=master)](https://dev.azure.com/skoruba/IdentityServer4.Admin/_build/latest?definitionId=2?branchName=master)


The application is written in the **.Net Core - using .NET Core 3.1**


## Dependencies
To install it, you'll need to satisfy the following dependencies:

* [.NET Core](https://dotnet.microsoft.com/download)

Installing with APT can be done with a few commands. Before you install .NET, run the following commands to add the Microsoft package signing key to your list of trusted keys and add the package repository.
Open a terminal and run the following commands:

#####On Ubuntu 20.04 

	wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
	sudo dpkg -i packages-microsoft-prod.deb

#####On Ubuntu 18.04 

	wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
	sudo dpkg -i packages-microsoft-prod.deb


###Install the SDK
.NET Core SDK allows you to develop apps with .NET Core. If you install .NET Core SDK, you don't need to install the corresponding runtime. To install .NET Core SDK, run the following commands:

	sudo apt-get update; \
	sudo apt-get install -y apt-transport-https && \
	sudo apt-get update && \
	sudo apt-get install -y dotnet-sdk-3.1

###Install the runtime
The .NET Core Runtime allows you to run apps that were made with .NET Core that didn't include the runtime. The commands below install the ASP.NET Core Runtime, which is the most compatible runtime for .NET Core. In your terminal, run the following commands.

	sudo apt-get update; \
  	sudo apt-get install -y apt-transport-https && \
  	sudo apt-get update && \
  	sudo apt-get install -y aspnetcore-runtime-3.1


## Compile and install
Once you have installed all the dependencies, get the code:

	git clone https://github.com/freehand-dev/aes67discovery.git
	cd aes67discovery

Then just use:

	sudo mkdir /opt/aes67discovery/bin
	dotnet restore
	dotnet build
	sudo dotnet publish --runtime linux-x64 --output /opt/aes67discovery/bin -p:PublishSingleFile=true -p:PublishTrimmed=true ./aes67discovery

Install as daemon
   
	sudo nano /etc/systemd/system/aes67discovery.service

The content of the file will be the following one

	[Unit]
	Description=AES67 Discovery Server

	[Service]
	Type=notify
	WorkingDirectory=/opt/aes67discovery/etc/aes67discovery
	Restart=always
	RestartSec=10
	KillSignal=SIGINT
	ExecStart=/opt/aes67discovery/bin/aes67discovery
	Environment=ASPNETCORE_ENVIRONMENT=Production 

	[Install]
	WantedBy=multi-user.target

Add daemon to startup

	sudo systemctl daemon-reload
	sudo systemctl start aes67discovery
	sudo systemctl status aes67discovery
	sudo systemctl enable aes67discovery


## Configure and start
To start the server, you can use the `aes67discovery` executable as the application or `sudo systemctl start aes67discovery` as a daemon. For configuration you can edit a configuration file:

	sudo nano /opt/aes67discovery/etc/aes67discovery/aes67discovery.conf

The content of the file will be the following one

	[SAP]
	Interval=30000
	NetInterface="10.10.32.45"
	SDPFiles:0="/opt/aes67discovery/etc/sdp/test.sdp"
	SDPFiles:1="/opt/aes67discovery/etc/sdp/gstreamer.sdp"

	[Logging:LogLevel]
	Default=Information
	Microsoft=Warning


Developed by [Oleksandr Nazaruk](https://github.com/freehand-dev)
