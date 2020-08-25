AES67 Discovery Server
======

> aes67discovery

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-brightgreen.svg)](COPYING)
[![Build Status](https://dev.azure.com/oleksandr-nazaruk/aes67discovery/_apis/build/status/aes67discovery-CI)](https://dev.azure.com/oleksandr-nazaruk/aes67discovery/_apis/build/status/aes67discovery-CI)


AES67Discovery application help produce SAP announcements for discovery AES67 audio streams.

The application is written in the **.Net Core - using .NET Core 3.1**


## Dependencies
To install it, you'll need to satisfy the following dependencies:

* [.NET Core](https://dotnet.microsoft.com/download)

Installing with APT can be done with a few commands. Before you install .NET, run the following commands to add the Microsoft package signing key to your list of trusted keys and add the package repository.
Open a terminal and run the following commands:

<b>On Ubuntu 20.04</b>

	wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
	sudo dpkg -i packages-microsoft-prod.deb

<b>On Ubuntu 18.04</b>

	wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
	sudo dpkg -i packages-microsoft-prod.deb


<b>Install the SDK</b>

.NET Core SDK allows you to develop apps with .NET Core. If you install .NET Core SDK, you don't need to install the corresponding runtime. To install .NET Core SDK, run the following commands:

	sudo apt-get update; \
	sudo apt-get install -y apt-transport-https && \
	sudo apt-get update && \
	sudo apt-get install -y dotnet-sdk-3.1

<b>Install the runtime</b>

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

## Usage

Create SDP file for stream

   	v=0
	o=- 1 0 IN IP4 10.10.32.2
	s=AES67Discovery - AudioTest
	i=2 channels: R, L
	c=IN IP4 239.69.45.100/32
	t=0 0
	a=recvonly
	m=audio 5004 RTP/AVP 97
	a=rtpmap:97 L24/48000/2
	a=ptime:1
	a=ts-refclk:ptp=IEEE1588-2008:00-1D-C1-FF-FE-0E-49-64:0
	a=mediaclk:direct=0


Run audio stream

	gst-launch-1.0 -v audiotestsrc ! queue ! audioresample ! audio/x-raw,channels=2,rate=48000 ! rtpL24pay pt=97 ! udpsink host=239.69.45.100 port=5004 auto-multicast=true multicast-iface=eno2


Developed by [Oleksandr Nazaruk](https://github.com/freehand-dev)
