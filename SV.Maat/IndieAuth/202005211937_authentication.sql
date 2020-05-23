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
    "ClientName" character varying(128) COLLATE pg_catalog."default",
    "ClientLogo" character varying(128) COLLATE pg_catalog."default",
    "Scope"  character varying(128) COLLATE pg_catalog."default",
    "AuthCodeExpiresAt" timestamp with time zone,
    "State" character varying(128) COLLATE pg_catalog."default",
    "AccessToken" character varying(128) COLLATE pg_catalog."default",
    "UserId" integer
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.authenticationrequests
    OWNER to gilmae;


-- Table: public.accesstokens

-- DROP TABLE public.accesstokens;

CREATE TABLE public.accesstokens
(
    id integer NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
    "AuthenticationRequestId" integer NOT NULL,
    "UserId" integer NOT NULL,
    "Name" character varying(128) COLLATE pg_catalog."default" NOT NULL,
    "ClientId" character varying(128) COLLATE pg_catalog."default" NOT NULL,
    "Scope" character varying(128) COLLATE pg_catalog."default" NOT NULL,
    "Salt"  character varying(64) COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT accesstokens_pkey PRIMARY KEY (id)
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.accesstokens
    OWNER to gilmae;