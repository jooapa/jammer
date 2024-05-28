sudo apt update
sudo apt install -y git dotnet7 libfuse2

git clone https://github.com/jooapa/jammer.git ./jammer && \
    cd jammer && \
    dotnet restore && \
    cd jammer && \
    build.sh
