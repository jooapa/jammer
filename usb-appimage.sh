sudo apt install -y git dotnet7 libfuse2

git clone https://github.com/jooapa/jammer.git . && \
    dotnet restore && \
    ./build.sh