import { useEffect, useState } from "react";
import PatientListElement from "../components/PatientListElement";
import axios from "axios";
import ErrorBox from "../components/ErrorBox";


export default function PatientListPage() {
    
    const [patients, setPatients] = useState([]);
    const [errorFlag, setErrorFlag] = useState(false);
    const [errorMsg, setErrorMsg] = useState("");
    
    useEffect(() => {
        getPatients();
    }, []);
    
    
    const getPatients = () => {
        axios.get("/Patients/list").then(response => {
        setPatients(response.data);
        }).catch(error => console.log(error));
    };

    const setError = (msg) => {
        getPatients();
        setErrorMsg(msg);
        setErrorFlag(true);
    };

    const clearErrorFlag = () => {
        setErrorMsg("");
        setErrorFlag(false);
    };
  
    return (
        <div className="relative overflow-x-auto shadow-md sm:rounded-lg w-9/12 m-auto mt-16">
            <ErrorBox changeErrorFlag={clearErrorFlag} errorFlag={errorFlag} errorMsg={errorMsg}/>
            <table className="w-full text-sm text-left rtl:text-right text-gray-500 ">
                <thead className="text-xs text-gray-700 uppercase bg-gray-50  ">
                    <tr>
                        <th scope="col" className="px-6 py-3">
                            Name
                        </th>
                        <th scope="col" className="px-6 py-3">
                            Email
                        </th>
                        <th scope="col" className="px-6 py-3">
                            Activated
                        </th>
                        <th scope="col" className="px-6 py-3">
                            <span className="sr-only">Edit</span>
                        </th>
                    </tr>
                </thead>
                <tbody>
                    {patients.map(item => (<PatientListElement 
                        id={item.id} 
                        firstName={item.firstName} 
                        surname={item.surname} 
                        email={item.email} 
                        activated={item.active}
                        concurrencyStamp={item.concurrencyStamp} 
                        setErrorFunc={setError}/>))
                    }
                </tbody>
            </table>
        </div>
    );
}