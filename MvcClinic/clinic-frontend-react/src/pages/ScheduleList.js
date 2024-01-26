import { useEffect, useState } from "react";
import ScheduleListElement from "../components/ScheduleListElement";
import axios from "axios";
import ErrorBox from "../components/ErrorBox";
import ButtonAccept from "../components/ButtonAccept";


export default function ScheduleListPage() {
    
    const [schedules, setSchedules] = useState([]);
    const [errorFlag, setErrorFlag] = useState(false);
    const [errorMsg, setErrorMsg] = useState("");

    useEffect(() => {
        getSchedules();
    }, []);    
    
    const getSchedules = () => {
        let headerParams = "?dateFrom=" + (new Date(1995, 11, 17)).toISOString();
        headerParams += "&dateTo=" + (new Date(2025, 1, 1)).toISOString();
        // headerParams += "?specialityId=3";
        axios.get("/Schedules/list"+headerParams).then(response => {
            console.log(response.data);
            setSchedules(response.data);
        }).catch(error => console.log(error));
    };


    const setError = (msg) => {
        setErrorMsg(msg);
        setErrorFlag(true);
    };

    const clearErrorFlag = () => {
        setErrorMsg("");
        setErrorFlag(false);
    };

    const handleDelete = () => {
        window.location.reload();
    };
  
    return (
        <div className="relative overflow-x-auto shadow-md sm:rounded-lg w-9/12 m-auto mt-16">
            <ButtonAccept onClick={() => {window.location.href = "/schedules/create"}} text={"Create"} className={"w-20 m-2"}/>
            <ErrorBox changeErrorFlag={clearErrorFlag} errorFlag={errorFlag} errorMsg={errorMsg}/>
            <table className="w-full text-sm text-left rtl:text-right text-gray-500 ">
                <thead className="text-xs text-gray-700 uppercase bg-gray-50  ">
                    <tr>
                        <th scope="col" className="px-6 py-3">
                            Date
                        </th>
                        <th scope="col" className="px-6 py-3">
                            Doctor
                        </th>
                        <th scope="col" className="px-6 py-3">
                            Patient
                        </th>
                        <th scope="col" className="px-6 py-3">
                            <span className="sr-only">Edit</span>
                        </th>
                    </tr>
                </thead>
                <tbody>
                    {schedules.map(item => (<ScheduleListElement 
                        id={item.id} 
                        doctorName={item.doctorName}
                        patientName={item.patientName}
                        date={item.date}
                        concurrencyStamp={item.concurrencyStamp} 
                        setErrorFunc={setError}
                        onDelete={handleDelete}
                        key={item.id}
                        />))
                    }
                </tbody>
            </table>
        </div>
    );
}