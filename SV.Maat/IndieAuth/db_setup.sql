-- Table: public.authenticationrequests

-- DROP TABLE public.authenticationrequests;

CREATE TABLE public.authenticationrequests
(
    id integer NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
    "UserProfileUrl" character varying(128) COLLATE pg_catalog."default",
    "ClientId" character varying(128) COLLATE pg_catalog."default" NOT NULL,
    "RedirectUri" character varying(256) COLLATE pg_catalog."default",
    "CsrfToken" character varying(256) COLLATE pg_catalog."default",
    "ResponseType" character varying(128) COLLATE pg_catalog."default" NOT NULL,
    "AuthorisationCode" character varying(128) COLLATE pg_catalog."default",
    "AuthCodeExpiresAt" date,
    "State" character varying(128) COLLATE pg_catalog."default"
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.authenticationrequests
    OWNER to gilmae;