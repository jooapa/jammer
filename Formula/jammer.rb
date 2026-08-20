class Jammer < Formula
  desc "Lightweight, cross-platform terminal music player"
  homepage "https://github.com/jooapa/jammer"
  version "3.53"
  license "Proprietary"

  on_macos do
    if Hardware::CPU.arm?
      url "https://github.com/jooapa/jammer/releases/download/#{version}/jammer-#{version}-osx-arm64.tar.gz"
      sha256 "557b76e07945299abb777d54a149f93e036856402d1298ee76844cd099f874ac"
    else
      url "https://github.com/jooapa/jammer/releases/download/#{version}/jammer-#{version}-osx-x64.tar.gz"
      sha256 "8b5ee86c2812eada4a1ae806a97f39fbe8062b3d56a6e936ba3cef5d12f0b9ff"
    end
  end

  on_linux do
    if Hardware::CPU.arm?
      url "https://github.com/jooapa/jammer/releases/download/#{version}/jammer-#{version}-arm64.tar.gz"
      sha256 "359771da713cf6ab399874e6476e073415d5501c7560b61beeb0d67d1a71d9a0"
    else
      url "https://github.com/jooapa/jammer/releases/download/#{version}/jammer-#{version}-x86_64.tar.gz"
      sha256 "882797cc492a66e8d6b2e0fadfd06850144361f86766ba28dc67eddac2e570a1"
    end
  end

  def install
    if OS.mac?
      system "xattr", "-cr", "."
      system "codesign", "--force", "--sign", "-", "Jammer.bin"
    end

    libexec.install Dir["*"]
    executable = OS.mac? ? "Jammer.bin" : "Jammer.CLI"

    (bin/"jammer").write_env_script libexec/executable,
      DYLD_LIBRARY_PATH: "#{libexec}${DYLD_LIBRARY_PATH:+:$DYLD_LIBRARY_PATH}",
      LD_LIBRARY_PATH: "#{libexec}${LD_LIBRARY_PATH:+:$LD_LIBRARY_PATH}"
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