import sys, os

# goto parent directory of the python script
os.chdir(os.path.dirname(os.path.abspath(__file__)))
os.chdir('..')

args = sys.argv[1:]
if(len(args) == 0):
    print('Usage: update.py <major0,1><minor0,1><patch 0,1>')
    sys.exit(1)
if args[0][0:1] == "1":
    is_major = True
elif args[0][0:1] == "0":
    is_major = False
    
if args[0][1:2] == "1":
    is_minor = True
elif args[0][1:2] == "0":
    is_minor = False

if args[0][2:3] == "1":
    is_patch = True
elif args[0][2:3] == "0":
    is_patch = False
    

with open('VERSION', 'r') as version_file:
    version = version_file.read()
    major = version.split('.')[0]
    minor = version.split('.')[1]
    patch = version.split('.')[2]
    build = version.split('.')[3]

    if is_major:
        major = int(major) + 1
    if is_minor:
        minor = int(minor) + 1
    if is_patch:
        patch = int(patch) + 1   
    build = int(build) + 1
    new_version = f"{major}.{minor}.{patch}.{build}"
    os.system(f'python change_version.py "{new_version}"')
    
    print(f'Version updated to {new_version}')
