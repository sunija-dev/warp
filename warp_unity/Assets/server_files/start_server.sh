#!/bin/sh

while(true)
do
	if [ -f "SERVER_LINUX.zip" ] 
	then
		unzip SERVER_LINUX.zip
		rm SERVER_LINUX.zip
	fi
	chmod u+x warp_server.x86_64

	now=$(date +'%Y_%m_%d_%H_%M_%S')

	./warp_server.x86_64 -logfile "log_$now.txt" -batchmode
	
	echo "Server closed itself."
	sleep 10
done

