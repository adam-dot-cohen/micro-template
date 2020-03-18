@echo off

if exist %1\setup.py (
	python "%1\setup.py" sdist
) else (
	echo "%1\setup.py does not exist"
)
