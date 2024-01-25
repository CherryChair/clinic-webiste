import React from 'react';
import { Navigate } from 'react-router-dom';
import 'jwt-decode'
import { isAdmin, isDoctor, isLoggedIn, isPatient } from '../pages/Login';
 
const RouteGuard = ({ adminComponent, doctorComponent, patientComponent, loggedIn, component: Component, ...rest }) => {
   let redirect = false;
   let redirectAdmin = false;
   let redirectDoctor = false;
   let redirectPatient = false;
   if(loggedIn && !isLoggedIn()) redirect = true;
   if(adminComponent && !isAdmin()) redirectAdmin = true;
   if(patientComponent && !isPatient()) redirectPatient = true;
   if(doctorComponent && !isDoctor()) redirectDoctor = true;
   if (!redirect) {
      if (adminComponent && patientComponent && doctorComponent) {
         redirect = redirectAdmin && redirectDoctor && redirectPatient;
      } else if (adminComponent && patientComponent) {
         redirect = redirectAdmin && redirectPatient;
      } else if (adminComponent && doctorComponent) {
         redirect = redirectAdmin && redirectDoctor;
      } else if (doctorComponent && patientComponent) {
         redirect = redirectDoctor && redirectPatient;
      } else {
         redirect = redirectAdmin || redirectDoctor || redirectPatient;
      }
   }

   return redirect ? <Navigate to={{pathname: '/home'}} replace/> : <Component {...rest}/>; 
};
 
export default RouteGuard;