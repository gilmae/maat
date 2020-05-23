-- Table: public."Syndications"

-- DROP TABLE public.UserSyndicationss;

CREATE TABLE public.Syndications
(
    Id integer NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
    "AccountName" character varying(128) COLLATE pg_catalog."default",
    "Network" character varying(128) COLLATE pg_catalog."default" NOT NULL,
    "UserId" int
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.Syndications
    OWNER to gilmae;