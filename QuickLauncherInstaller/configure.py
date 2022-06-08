import sys
import os
import subprocess
import re
import shutil
from string import Template


INSTALLER = 'installer.iss'
INSTALLER_TPL = 'installer_tpl.iss'

def parse_config(argv):
	if not isinstance(argv, list) or len(argv) < 2:
		raise Exception(f"invalid arguments: {argv}")
	
	config = argv[1]
	assert config in ['Debug', 'Release'], f"Configuration must be Debug or Release"
	return config


def find_version(config):
	path = fr'..\QuickLauncher\bin\x64\{config}\netcoreapp3.1\QuickLauncher.exe'
	abs_path = os.path.abspath(path)
	cmd = f'(Get-Command "{abs_path}").FileVersionInfo.FileVersion'
	print(f'cmd={cmd}')
	result = subprocess.run(['powershell', cmd], stdout=subprocess.PIPE)
	version =  result.stdout.decode('utf-8')
	return version.strip()


def read_installer():
	with open(INSTALLER_TPL, 'r', encoding='utf-8') as fp:
		return fp.read()


def write_installer(installer):
	with open(INSTALLER, 'w', encoding='utf-8') as fp:
		fp.write(installer)


if __name__ == '__main__':
	config = parse_config(sys.argv)
	version = find_version(config)
	if not re.match(r'\d+\.\d+\.\d+\.\d+', version):
		print("unable to find version")
		sys.exit(1)
	print(f'version={version}')

	print("read installer.iss")
	installer = read_installer()
	if not installer:
		print("unable to read installer.iss")
		sys.exit(2)

	tpl = Template(installer)
	installer = tpl.substitute(dict(VERSION=version, CONFIG=config))
	print("write installer.iss")
	write_installer(installer)
	if os.path.exists('Output'):
		print("removing previous Output directory")
		shutil.rmtree('Output')