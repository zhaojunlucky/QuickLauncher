import xml.etree.ElementTree as ET
import sys

def parse_version():
    tree = ET.parse('./QuickLauncher/QuickLauncher.csproj')
    root = tree.getroot()
    for child in root:
        if child.tag == 'PropertyGroup':
            for prop in child:
                if prop.tag == 'Version':
                    return prop.text
    sys.exit(1)


if __name__ == '__main__':
    print(parse_version())
