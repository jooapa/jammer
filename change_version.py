import os, sys

if len(sys.argv) < 2:
    print("Usage: python change_version.py <version>")
    sys.exit(1)

# Write the version to the VERSION file
with open('VERSION', 'w') as version_file:
    version_file.write(sys.argv[1])

# Read the lines from the NSIS file
with open(os.path.join('nsis', 'setup.nsi'), 'r') as nsis_file:
    lines = nsis_file.readlines()

# Write the lines back to the NSIS file, replacing the version number
with open(os.path.join('nsis', 'setup.nsi'), 'w') as new_nsis_file:
    for line in lines:
        if line.startswith('!define VERSION'):
            new_nsis_file.write('!define VERSION "' + sys.argv[1] + '"\n')
        else:
            new_nsis_file.write(line)
            
with open(os.path.join('src', "Jammer", 'Utils.cs'), 'r') as utils_file:
    lines = utils_file.readlines()
    
with open(os.path.join('src', "Jammer", 'Utils.cs'), 'w') as new_utils_file:
    for line in lines:
        if line.startswith('        public static string version = "'):
            new_utils_file.write('        public static string version = "' + sys.argv[1] + '";\n')
        else:
            new_utils_file.write(line)

with open(os.path.join('Jammer.csproj'), 'r') as Jammer_file:
    lines = Jammer_file.readlines()
    
with open(os.path.join('Jammer.csproj'), 'w') as new_Jammer_file:
    for line in lines:
        if line.startswith('      <Version>'):
            new_Jammer_file.write('      <Version>' + sys.argv[1] + '</Version>\n')
        else:
            new_Jammer_file.write(line)


with open(os.path.join('build.bat'), 'r') as Jammer_file:
    lines = Jammer_file.readlines()
    
with open(os.path.join('build.bat'), 'w') as new_Jammer_file:
    for line in lines:
        if line.startswith('SET "start_name=Jammer-Setup_'):
            new_Jammer_file.write('SET "start_name=Jammer-Setup_V' + sys.argv[1] + '.exe\"\n')
        else:
            new_Jammer_file.write(line)