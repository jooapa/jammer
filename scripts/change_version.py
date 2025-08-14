import os
import argparse


def replace_in_file(file_path, start_string, replacement_line):
    with open(file_path, 'r') as file:
        lines = file.readlines()

    with open(file_path, 'w') as file:
        for line in lines:
            if line.startswith(start_string):
                file.write(replacement_line)
            else:
                file.write(line)


def change_version(version):
    # Write the version to the VERSION file
    with open('VERSION', 'w') as version_file:
        version_file.write(version)

    # Update files
    replace_in_file(os.path.join('nsis', 'setup.nsi'),
                    '!define VERSION',
                    f'!define VERSION "{version}"\n')

    replace_in_file(os.path.join('Jammer.Core', 'src', 'Utils.cs'),
                    '        public static string Version = "',
                    f'        public static string Version = "{version}";\n')

    replace_in_file(os.path.join('Jammer.CLI', 'Jammer.CLI.csproj'),
                    '    <Version>',
                    f'    <Version>{version}</Version>\n')

    replace_in_file(os.path.join('Jammer.CLI', 'buildcli.bat'),
                    'SET "start_name=Jammer-Setup_',
                    f'SET "start_name=Jammer-Setup_V{version}.exe\"\n')


if __name__ == '__main__':
    os.chdir(os.path.dirname(os.path.abspath(__file__)))
    os.chdir('..')
    
    parser = argparse.ArgumentParser()
    parser.add_argument('version', help='New version number')
    args = parser.parse_args()

    change_version(args.version)
