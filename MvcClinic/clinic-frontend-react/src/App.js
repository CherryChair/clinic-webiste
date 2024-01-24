import './App.css';
import './routes'
import Cookies from 'js-cookie';
import './components/RouteGuard'
import MyRoutes from './routes';
import { setAuthToken } from './pages/Login';
import Navbar from './components/Navbar';

const token = Cookies.get("token");
if (token) {
    setAuthToken(token);
}

function App() {
  return (
    <>
      <Navbar/>
      <div className="App">
        <MyRoutes></MyRoutes>
      </div>
    </>
  );
}

export default App;
