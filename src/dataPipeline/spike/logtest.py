import logging
import logging.config
import yaml

with open('logging.yml', 'r') as f:
    log_cfg = yaml.safe_load(f.read())

logging.config.dictConfig(log_cfg)

logger = logging.getLogger()
logger.debug('debug message')
logger.error('error message')