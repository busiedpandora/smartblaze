#!/bin/bash

cd SmartBlaze.Backend
./SmartBlaze.Backend &

sleep 2

cd ../SmartBlaze.Frontend
./SmartBlaze.Frontend &

sleep 2

xdg-open https://localhost:7040 || open https://localhost:7040
