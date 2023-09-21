﻿using Agent.Agency.Models;
using Agent.Agency.Base.Web.Http;
using Agent.DataAccess;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Profile;
using System.Web.Security;
using System.Web.SessionState;
using Agent.Agency.Base.Data;
using System.Web.Providers.Entities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;


namespace Agent.Agency.Controllers
{
    public class AccomodationsController : BaseApiController, IBaseActions
    {        
        #region IBaseActions - Default CRUD Actions


        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetAll()
        {
            if (ModelState.IsValid)
            {
                BaseRepository bs = new BaseRepository();
                List<Accomodation> accomodationList = bs.GetAllAccomodations();
                return Ok(accomodationList, HttpStatusCode.OK, "Successfully GetAll");
            }
            else
            {
                return Error(HttpStatusCode.NotAcceptable, ModelState);
            }
        }


        [System.Web.Http.HttpGet]
        public HttpResponseMessage Get(Guid objId)
        {
            if (ModelState.IsValid)
            {
                BaseRepository bs = new BaseRepository();

                Accomodation accomodation = bs.GetSelectedAccomodation(objId);
                return Ok(accomodation, HttpStatusCode.OK, "Successfully Get");
            }
            else
            {
                return Error(HttpStatusCode.NotAcceptable, ModelState);
            }
        }


        [System.Web.Http.HttpPost]
        public HttpResponseMessage Create(dynamic model)
        {
            Accomodation accomodation = MapJsonToModelObject<Accomodation>(model, false);

            if (ModelState.IsValid)
            {
                BaseRepository bs = new BaseRepository();

                accomodation.AccomodationId = Guid.NewGuid();


                //Create accomodation in backend app
                MasterBService.Accomodation backendAccomodation = AutoMapper.Mapper.Map<MasterBService.Accomodation>(accomodation); //map accomodation to master backend accomodation
                MasterBService.MasterBackendServiceSoapClient client = new MasterBService.MasterBackendServiceSoapClient();

                client.CreateAccomodationAndAssignOwner(backendAccomodation, GetUserId());

                //Create accomodation on our local app
                bs.CreateAccomodation(accomodation);
                bs.AssingAccomodationOwner(GetUserId(), accomodation.AccomodationId);

                return Ok(accomodation, HttpStatusCode.OK, "Successfully created");
                // return Ok(null, HttpStatusCode.OK, "Successfully created");
            }
            else
            {
                return Error(HttpStatusCode.NotAcceptable, ModelState);
            }
        }
   
        [System.Web.Http.HttpPut]
        public HttpResponseMessage Update(dynamic model)
        {
            Accomodation accomodation = MapJsonToModelObject<Accomodation>(model, false);

            if (ModelState.IsValid)
            {
                BaseRepository bs = new BaseRepository();

                MasterBService.Accomodation backendAccomodation = AutoMapper.Mapper.Map<MasterBService.Accomodation>(accomodation); //map accomodation to master backend accomodation
                MasterBService.MasterBackendServiceSoapClient client = new MasterBService.MasterBackendServiceSoapClient();

                client.UpdateAccomodation(backendAccomodation);


                bs.UpdateAccomodation(accomodation);
                return Ok(null, HttpStatusCode.OK, "Successfully updated");
            }
            else
            {
                return Error(HttpStatusCode.NotAcceptable, ModelState);
            }
        }

        [System.Web.Http.HttpDelete]
        public HttpResponseMessage Delete(Guid objId)
        {
            if (ModelState.IsValid)
            {
                BaseRepository bs = new BaseRepository();
                bs.DeleteAccomodation(objId);
                return Ok(null, HttpStatusCode.OK, "Successfully deleted");
            }
            else
            {
                return Error(HttpStatusCode.NotAcceptable, ModelState);
            }
        }

        #endregion

        #region Custom Actions

        [System.Web.Http.HttpGet]
        public HttpResponseMessage getSelectedAccomodations(int accomodationType)
        {
            if (ModelState.IsValid)
            {
                BaseRepository bs = new BaseRepository();

                List<Accomodation> accomodationList = bs.GetAccomodationsForSelectedType(accomodationType);
                return Ok(accomodationList, HttpStatusCode.OK, "Successfully Get");
            }
            else
            {
                return Error(HttpStatusCode.NotAcceptable, ModelState);
            }
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage getAccomodationsForSelectedLocation(int objId)
        {
            if (ModelState.IsValid)
            {
                BaseRepository bs = new BaseRepository();

                List<Accomodation> accomodationList = bs.GetAccomodationsForSelectedLocation(objId);
                return Ok(accomodationList, HttpStatusCode.OK, "Successfully Get");
            }
            else
            {
                return Error(HttpStatusCode.NotAcceptable, ModelState);
            }
        }


        [System.Web.Http.HttpGet]
        public HttpResponseMessage getAccomodationsForSelectedOwner(Guid objId)
        {
            if (ModelState.IsValid)
            {
                BaseRepository bs = new BaseRepository();

                //First retreive fresh ratings
                MasterBService.MasterBackendServiceSoapClient client = new MasterBService.MasterBackendServiceSoapClient();
                MasterBService.Rating[] ratingListFromMB = client.GetRatingsForOwnerAccomodations(GetUserId());

                List<Rating> freshRatings = AutoMapper.Mapper.Map<MasterBService.Rating[], List<Rating>>(ratingListFromMB);
                bs.RefreshRatingsTable(freshRatings);

                List<AccomodationOwner> accomodationList = bs.GetAccomodationsForSelectedUser(GetUserId());

                accomodationList.ForEach(a => a.Accomodation.Ratings = bs.GetRatingsForAccomodationId(a.Accomodation.AccomodationId));


                return Ok(accomodationList, HttpStatusCode.OK, "Successfully Get");
            }
            else
            {
                return Error(HttpStatusCode.NotAcceptable, ModelState);
            }
        }

        #endregion
    }
}