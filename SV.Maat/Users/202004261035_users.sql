-- Table: public."User"

-- DROP TABLE public.Users;

CREATE TABLE public.Users
(
    Id integer NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
    "Name" character varying(128) COLLATE pg_catalog."default",
    "Username" character varying(128) COLLATE pg_catalog."default" NOT NULL,
    "Email" character varying(128) COLLATE pg_catalog."default",
    "Url" character varying(256) COLLATE pg_catalog."default",
    "HashedPassword" character varying(128) COLLATE pg_catalog."default" NOT NULL
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.users
    OWNER to gilmae;