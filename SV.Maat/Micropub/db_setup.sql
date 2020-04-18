-- Table: public.events

-- DROP TABLE public.events;

CREATE TABLE public.events
(
    id integer NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
    aggregate_id uuid,
    type character varying(128) COLLATE pg_catalog."default",
    body text COLLATE pg_catalog."default",
    event_type character varying(128) COLLATE pg_catalog."default",
    version integer NOT NULL,
    CONSTRAINT events_pkey PRIMARY KEY (id)
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.events
    OWNER to gilmae;