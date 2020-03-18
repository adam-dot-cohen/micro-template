from setuptools import setup, find_packages

setup(
    name='data_router',
    version='1.0',
    description='Data routing module',
    author='j.keane',
    author_email='j.keane@laso.com',
    
    packages=['framework','framework.pipeline','steplibrary'],
    scripts=['accept_processor.__main__.py','accept_processor.AcceptProcessory.py'],
    package_data={
        "": ["*.msg"],
    }
)
