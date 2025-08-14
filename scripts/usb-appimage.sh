sudo apt update
sudo add-apt-repository ppa:dotnet/dotnet8
sudo apt install -y git dotnet8 libfuse2

git clone https://github.com/jooapa/jammer.git ./jammer && \
    cd jammer && \
    dotnet restore && \
    cd jammer && \
    build.sh
