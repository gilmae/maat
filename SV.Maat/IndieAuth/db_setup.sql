-- Table: public."AuthenticationRequests"

-- DROP TABLE public.AuthenticationRequests;

CREATE TABLE public.AuthenticationRequests
(
    Id integer NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
    "UserProfileUrl" character varying(128) COLLATE pg_catalog."default",
    "ClientId" character varying(128) COLLATE pg_catalog."default" NOT NULL,
    "RedirectUri" character varying(256) COLLATE pg_catalog."default",
    "CsrfToken" character varying(256) COLLATE pg_catalog."default",
    "ResponseType" character varying(128) COLLATE pg_catalog."default" NOT NULL
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.AuthenticationRequests
    OWNER to gilmae;