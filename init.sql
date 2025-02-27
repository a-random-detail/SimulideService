CREATE ROLE simulide WITH LOGIN PASSWORD 'simulide-pw';
ALTER ROLE simulide CREATEDB;
CREATE DATABASE simulide-db OWNER simulide;
