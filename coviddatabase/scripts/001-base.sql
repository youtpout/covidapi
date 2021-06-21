-- Database: covid

-- DROP DATABASE covid;

CREATE DATABASE covid
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'French_France.1252'
    LC_CTYPE = 'French_France.1252'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1;

    
CREATE TABLE public.news
(
    title text,
    content text,
    text_source text,
    source text,
    lang text,
    date_create TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    date_update TIMESTAMP WITH TIME ZONE,
    id serial primary key
);

CREATE TABLE public.country
(
    name text NOT NULL,
    three text NOT NULL,
    two text NOT NULL,
    code text NOT NULL,
    id serial primary key
);


CREATE TABLE public.cities
(
    city text NOT NULL,
    extras text,
    latitude numeric(12,6) NOT NULL,
    longitude numeric(12,6) NOT NULL,
    confirmed integer NOT NULL,
    recovered integer NOT NULL,
    deaths integer NOT NULL,
    id serial primary key,
    country_id integer references public.country(id) NOT NULL,
    date_create TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,
    date_update TIMESTAMP WITH TIME ZONE,
    unique(country_id,latitude,longitude)
);

CREATE TABLE public.province
(
    country text NOT NULL,
    province text,
    latitude numeric(12,6) NOT NULL,
    longitude numeric(12,6) NOT NULL,
    id serial primary key,
    unique(country,province,latitude,longitude)
);

CREATE TABLE public.serie
(
    date date NOT NULL,
    confirmed integer,
    recovered integer,
    deaths integer,
    province_id integer references public.province(id) NOT NULL,
    date_create TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,
    date_update TIMESTAMP WITH TIME ZONE,
    dont_update boolean NOT NULL,
    id serial primary key,
    unique(province_id,date)
);

CREATE TABLE public.logs
(
    date TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,
    level text NOT NULL,
    category text NOT NULL,
    message text NOT NULL,
    event_id text NOT NULL,
    username text NOT NULL,
    id serial primary key
);

