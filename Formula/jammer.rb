class Jammer < Formula
  desc "Lightweight, cross-platform terminal music player"
  homepage "https://github.com/jooapa/jammer"
  version "3.53"
  license "Proprietary"

  on_macos do
    if Hardware::CPU.arm?
      url "https://github.com/jooapa/jammer/releases/download/#{version}/jammer-#{version}-osx-arm64.tar.gz"
      sha256 "db9040a6200b45cbeedf627af7c2314a5a7893ca2d7ea928787a37a597b18e24"
    else
      url "https://github.com/jooapa/jammer/releases/download/#{version}/jammer-#{version}-osx-x64.tar.gz"
      sha256 "5c61ae3d1832339063496e78dec65d54161aa2da4ff5fa948a7a93522f4718be"
    end
  end

  on_linux do
    if Hardware::CPU.arm?
      url "https://github.com/jooapa/jammer/releases/download/#{version}/jammer-#{version}-arm64.tar.gz"
      sha256 "a486865927d59d5a8ef3714263a1ccb02b207e7396ceb319e1206a184b554daf"
    else
      url "https://github.com/jooapa/jammer/releases/download/#{version}/jammer-#{version}-x86_64.tar.gz"
      sha256 "9dd8e71cfda8024696ca28e231e907fdbd8c24fb2d15cb2373cb1144f49733cc"
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