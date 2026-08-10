class Jammer < Formula
  desc "Lightweight, cross-platform terminal music player"
  homepage "https://github.com/jooapa/jammer"
  version "3.53"
  license "MIT"

  on_macos do
    if Hardware::CPU.arm?
      url "https://github.com/jooapa/jammer/releases/download/#{version}/jammer-#{version}-osx-arm64.tar.gz"
      sha256 "2ca091a77464a91f29ff906ce09d282117f7f579de68bad66d74ecd2f48411b9"
    else
      url "https://github.com/jooapa/jammer/releases/download/#{version}/jammer-#{version}-osx-x64.tar.gz"
      sha256 "PLACEHOLDER_OSX_X64_SHA256"
    end
  end

  on_linux do
    if Hardware::CPU.arm?
      url "https://github.com/jooapa/jammer/releases/download/#{version}/jammer-#{version}-arm64.tar.gz"
      sha256 "PLACEHOLDER_LINUX_ARM64_SHA256"
    else
      url "https://github.com/jooapa/jammer/releases/download/#{version}/jammer-#{version}-x86_64.tar.gz"
      sha256 "PLACEHOLDER_LINUX_X64_SHA256"
    end
  end

  def install
    if OS.mac?
      lib.install "libbass.dylib", "libbassmidi.dylib", "libbassopus.dylib"
      bin.install "Jammer.bin"
      (bin/"jammer").write_env_script bin/"Jammer.bin",
        DYLD_LIBRARY_PATH: "#{lib}${DYLD_LIBRARY_PATH:+:$DYLD_LIBRARY_PATH}"
    else
      lib.install "libbass.so", "libbassmidi.so", "libbassopus.so"
      bin.install "Jammer.CLI"
      (bin/"jammer").write_env_script bin/"Jammer.CLI",
        LD_LIBRARY_PATH: "#{lib}${LD_LIBRARY_PATH:+:$LD_LIBRARY_PATH}"
    end
    (share/"jammer").install Dir["locales/*"]
  end

  def caveats
    <<~EOS
      Run jammer from any terminal:
        jammer

      Music and config are stored in ~/jammer/
    EOS
  end

  test do
    assert_match "Jammer", shell_output("#{bin}/jammer --version")
  end
end
