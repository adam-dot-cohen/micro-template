import fnmatch
from setuptools import find_packages, setup
from setuptools.command.build_py import build_py as build_py_orig


excluded = ['accept_processor/*.py']



class build_py(build_py_orig):
    def find_package_modules(self, package, package_dir):
        modules = super().find_package_modules(package, package_dir)
        
        new_modules = [
            (pkg, mod, file)
            for (pkg, mod, file) in modules
            if not any(fnmatch.fnmatchcase(file, pat=pattern) for pattern in excluded)
        ]
        print(new_modules)
        return new_modules

setup(
    name='data-router',
    version='1.0',
    description='Data routing module',
    author='j.keane',
    author_email='j.keane@laso.com',
    python_requires='>=3.7',
    
    packages=find_packages(include=['framework','framework.pipeline','steplibrary']),
    cmdclass={'build_py': build_py},
	package_dir={'framework': 'framework', 'steplibrary': 'steplibrary'},
    #scripts=['accept_processor/__main__.py','accept_processor/AcceptProcessor.py'],
    zip_safe=False,
)
