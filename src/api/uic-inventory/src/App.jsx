import { GraphQLClient, ClientContext } from 'graphql-hooks';
import memCache from 'graphql-hooks-memcache';
import Navigation from './Navigation';
import Profile from './Profile';
import { BrowserRouter as Router, Switch, Route, Link } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

const client = new GraphQLClient({
  url: '/graphql',
  cache: memCache(),
});

const contextClass = {
  success: "bg-green-400",
  error: "bg-red-600",
  info: "bg-gray-600",
  warning: "bg-orange-400",
  default: "bg-indigo-600",
  dark: "bg-white-600 font-gray-300",
};

function App() {
  return (
    <ClientContext.Provider value={client}>
      <Router>
        <Navigation />
        <Switch>
          <Route path="/contact">
          </Route>
          <Route path="/profile">
            <Profile />
          </Route>
          <Route path="/">
          </Route>
        </Switch>
      </Router>
      <ToastContainer
        toastClassName={({ type }) => contextClass[type || "default"] +
          " my-3 relative flex p-1 min-h-10 rounded-md justify-between overflow-hidden cursor-pointer"
        }
        bodyClassName={() => "text-sm font-white font-med block p-3"}
      />
    </ClientContext.Provider>
  );
}

export default App;
