sudo apt update
sudo apt install -y git dotnet7 libfuse2

echo "Cloning jammer repository and building it..."
git clone https://github.com/jooapa/jammer.git ./jammer && \
    cd jammer && \
    dotnet restore && \
    ./build.sh