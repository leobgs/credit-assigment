--
-- PostgreSQL database dump
--

\restrict ldVn46SEeOwAEj4i4RmX0cnfHfaiyNCrrkIQv3l4pN5Be9aNKOcrDO69DpjzcjZ

-- Dumped from database version 15.19 (Homebrew)
-- Dumped by pg_dump version 16.15 (Homebrew)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


ALTER TABLE public."__EFMigrationsHistory" OWNER TO postgres;

--
-- Name: pengajuan_kredit; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.pengajuan_kredit (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    plafon numeric NOT NULL,
    bunga numeric(5,2) NOT NULL,
    tenor integer NOT NULL,
    angsuran numeric NOT NULL,
    created_at timestamp without time zone DEFAULT now() NOT NULL,
    updated_at timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.pengajuan_kredit OWNER TO postgres;

--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20261012093602_InitialCreate	8.0.31
\.


--
-- Data for Name: pengajuan_kredit; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.pengajuan_kredit (id, plafon, bunga, tenor, angsuran, created_at, updated_at) FROM stdin;
11111111-1111-1111-1111-111111111111	100000000	12.00	60	2224444.47	2026-10-07 16:51:24.11732	2026-10-07 16:51:24.11732
22222222-2222-2222-2222-222222222222	50000000	10.00	36	1613359.54	2026-10-08 16:51:24.11732	2026-10-08 16:51:24.11732
33333333-3333-3333-3333-333333333333	250000000	9.50	120	3235532.80	2026-10-09 16:51:24.11732	2026-10-09 16:51:24.11732
44444444-4444-4444-4444-444444444444	15000000	14.00	12	1346929.56	2026-10-10 16:51:24.11732	2026-10-10 16:51:24.11732
55555555-5555-5555-5555-555555555555	500000000	8.75	180	4999655.22	2026-10-11 16:51:24.11732	2026-10-11 16:51:24.11732
66666666-6666-6666-6666-666666666666	30000000	11.50	24	1405257.65	2026-10-12 16:51:24.11732	2026-10-12 16:51:24.11732
\.


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: pengajuan_kredit PK_pengajuan_kredit; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pengajuan_kredit
    ADD CONSTRAINT "PK_pengajuan_kredit" PRIMARY KEY (id);


--
-- Name: idx_pengajuan_plafon; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_pengajuan_plafon ON public.pengajuan_kredit USING btree (plafon);


--
-- Name: idx_pengajuan_tenor; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_pengajuan_tenor ON public.pengajuan_kredit USING btree (tenor);


--
-- Name: idx_pengajuan_tenor_plafon; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_pengajuan_tenor_plafon ON public.pengajuan_kredit USING btree (tenor DESC, plafon DESC);


--
-- PostgreSQL database dump complete
--

\unrestrict ldVn46SEeOwAEj4i4RmX0cnfHfaiyNCrrkIQv3l4pN5Be9aNKOcrDO69DpjzcjZ

