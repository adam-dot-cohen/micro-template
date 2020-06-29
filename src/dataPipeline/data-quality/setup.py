__version__ = '0.1.115'

from setuptools import setup, find_packages
setup(
    name = "data-quality",
    version = __version__,
    packages = find_packages(exclude=["test_*.*"]),
    package_data={
        "": ["config/*.yml"]
        }
    )

